import * as vscode from 'vscode';
import { WorkspaceContextProvider } from './WorkspaceContextProvider';
import { WorkspaceApiService } from './WorkspaceApiService';

interface WorkspaceInfo {
    workspacePath: string | null;
    activeFilePath: string | null;
    lastFileChange: FileChangeInfo | null;
}

interface FileChangeInfo {
    path: string;
    type: string;
    workspaceFolder: string;
}

export class AgentChatPanel {
    public static currentPanel: AgentChatPanel | undefined;
    private _panel: vscode.WebviewPanel;
    private readonly _extensionUri: vscode.Uri;
    private _disposables: vscode.Disposable[] = [];
    private _apiService: WorkspaceApiService;
    private _workspaceInfo: WorkspaceInfo = {
        workspacePath: null,
        activeFilePath: null,
        lastFileChange: null
    };
    
    public static createOrShow(extensionUri: vscode.Uri) {
        const column = vscode.window.activeTextEditor
            ? vscode.window.activeTextEditor.viewColumn
            : undefined;
            
        if (AgentChatPanel.currentPanel) {
            AgentChatPanel.currentPanel._panel.reveal(column);
            return;
        }
        
        const panel = vscode.window.createWebviewPanel(
            'aiCodingAssistant',
            '🤖 AI Coding Assistant',
            column || vscode.ViewColumn.One,
            {
                enableScripts: true,
                retainContextWhenHidden: true,
                localResourceRoots: [extensionUri]
            }
        );
        
        AgentChatPanel.currentPanel = new AgentChatPanel(panel, extensionUri);
    }
   
  

    private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
        this._panel = panel;
        this._extensionUri = extensionUri;
        this._apiService = new WorkspaceApiService(WorkspaceApiService.getConfiguredBaseUrl());

        this._update();
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
        
        // Initialize workspace context - send current workspace to backend
        this._setupFileSystemWatcher();
        this._sendInitialWorkspaceInfo();
    }
    
    private _setupFileSystemWatcher() {
        const workspaceWatcher = vscode.workspace.createFileSystemWatcher('**/*');
        
        workspaceWatcher.onDidChange(uri => this._notifyFileChange('changed', uri));
        workspaceWatcher.onDidCreate(uri => this._notifyFileChange('created', uri));
        workspaceWatcher.onDidDelete(uri => this._notifyFileChange('deleted', uri));
        
        this._disposables.push(workspaceWatcher);

        // Watch for active editor changes
        vscode.window.onDidChangeActiveTextEditor(editor => {
            if (editor) {
                this._notifyActiveFileChange(editor.document.uri);
                this._workspaceInfo.activeFilePath = editor.document.uri.fsPath;                
            }
        }, null, this._disposables);

        // Send initial workspace info
        this._sendInitialWorkspaceInfo();
    }

    private _updateApiService() {
        this._apiService = new WorkspaceApiService(WorkspaceApiService.getConfiguredBaseUrl());
    }

    private _notifyFileChange(type: string, uri: vscode.Uri) {
        const fileChangeInfo = {
            type,
            path: uri.fsPath,
            workspaceFolder: vscode.workspace.getWorkspaceFolder(uri)?.uri.fsPath || ''
        };

        // Update local state for webview display
        this._workspaceInfo.lastFileChange = fileChangeInfo;        
    }

    private async _notifyActiveFileChange(uri: vscode.Uri) {        
        const success = await this._apiService.setActiveDocument(uri.fsPath);
        console.debug("Notifying completed for active file change "+uri.fsPath)
        if (!success) {
            console.error('Failed to notify active file change via REST API');
        }
    }

    private async _sendInitialWorkspaceInfo() {
        const workspaceFolders = vscode.workspace.workspaceFolders;
        const activeEditor = vscode.window.activeTextEditor;
        
        this._workspaceInfo = {
            workspacePath: workspaceFolders?.[0]?.uri.fsPath || null,
            activeFilePath: activeEditor?.document.uri.fsPath || null,
            lastFileChange: null
        };

        // Send workspace path to REST API
        if (this._workspaceInfo.workspacePath) {
            const success = await this._apiService.setWorkspacePath(this._workspaceInfo.workspacePath);
            if (!success) {
                console.error('Failed to send workspace path via REST API');
            }
        }

        // Send active document to REST API
        if (this._workspaceInfo.activeFilePath) {
            const success = await this._apiService.setActiveDocument(this._workspaceInfo.activeFilePath);
            if (!success) {
                console.error('Failed to send active document via REST API');
            }
        }       
    }
    
    private _update() {
        this._panel.webview.html = this._getHtmlForWebview();
    }
    
    private _getHtmlForWebview() {
        const baseUrl = WorkspaceApiService.getConfiguredBaseUrl();
        
        return `<!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src 'unsafe-inline'; script-src 'unsafe-inline'; frame-src ${baseUrl}; connect-src ${baseUrl};">
                <title>AI Coding Assistant</title>
                <style>
                    html, body {
                        margin: 0;
                        padding: 0;
                        height: 100vh;
                        background-color: var(--vscode-editor-background);
                        color: var(--vscode-editor-foreground);
                        font-family: var(--vscode-font-family);
                        font-size: var(--vscode-font-size);
                        overflow: hidden;
                    }
                    .container {
                        display: flex;
                        flex-direction: column;
                        height: 100vh;
                    }
                    .assistant-frame {
                        width: 100%;
                        height: 100%;
                        border: none;
                        background-color: var(--vscode-editor-background);
                    }
                    .loading {
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        height: 100%;
                        font-size: 14px;
                        color: var(--vscode-descriptionForeground);
                    }
                    .error {
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        height: 100%;
                        font-size: 14px;
                        color: var(--vscode-errorForeground);
                        flex-direction: column;
                        gap: 10px;
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="loading" id="loadingMessage">
                        🤖 Loading AI Coding Assistant...
                    </div>
                    <iframe
                        id="assistantFrame"
                        class="assistant-frame"
                        src="${baseUrl}?vscode=true"
                        title="AI Coding Assistant"
                        style="display: none;"
                        onload="onFrameLoad()"
                        onerror="onFrameError()">
                    </iframe>
                </div>
                
                <script>
                    function onFrameLoad() {
                        document.getElementById('loadingMessage').style.display = 'none';
                        document.getElementById('assistantFrame').style.display = 'block';
                    }
                    
                    function onFrameError() {
                        document.getElementById('loadingMessage').innerHTML = \`
                            <div class="error">
                                <div>❌ Failed to load AI Coding Assistant</div>
                                <div>Please check that the backend server is running at: ${baseUrl}</div>
                            </div>
                        \`;
                    }
                </script>
            </body>
            </html>`;
    }
    
    public reveal() {
        this._panel.reveal();
    }
    
    public dispose() {
        AgentChatPanel.currentPanel = undefined;
        
        this._panel.dispose();
        
        while (this._disposables.length) {
            const x = this._disposables.pop();
            if (x) {
                x.dispose();
            }
        }
    }
}