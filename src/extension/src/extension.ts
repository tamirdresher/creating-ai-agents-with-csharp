import * as vscode from 'vscode';
import { AgentChatPanel } from './AgentChatPanel';
import { WorkspaceContextProvider } from './WorkspaceContextProvider';

export function activate(context: vscode.ExtensionContext) {
    console.log('AI Coding Assistant extension is now active!');

    // Register assistant commands
    const assistantChatDisposable = vscode.commands.registerCommand(
        'aiCodingAssistant.openAssistant',
        () => AgentChatPanel.createOrShow(context.extensionUri)
    );
    
    context.subscriptions.push(
        assistantChatDisposable
    );

    // Workspace context provider
    const workspaceProvider = WorkspaceContextProvider.getInstance();
    
    // Listen for workspace changes
    const workspaceWatcher = vscode.workspace.onDidChangeWorkspaceFolders(() => {
        workspaceProvider.onWorkspaceChanged();
    });
    
    context.subscriptions.push(workspaceWatcher);
}


export function deactivate() {
    // Cleanup is handled by disposables
}