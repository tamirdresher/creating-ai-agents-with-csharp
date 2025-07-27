import * as vscode from 'vscode';
import { WorkspaceApiService } from './WorkspaceApiService';

export class WorkspaceContextProvider {
    private static instance: WorkspaceContextProvider;
    private _apiService: WorkspaceApiService;
    
    private constructor() {
        this._apiService = new WorkspaceApiService(WorkspaceApiService.getConfiguredBaseUrl());
    }
    
    public static getInstance(): WorkspaceContextProvider {
        if (!WorkspaceContextProvider.instance) {
            WorkspaceContextProvider.instance = new WorkspaceContextProvider();
        }
        return WorkspaceContextProvider.instance;
    }
    
    public async sendWorkspaceContext() {
        const workspaceFolders = vscode.workspace.workspaceFolders;
        if (workspaceFolders && workspaceFolders.length > 0) {
            const workspacePath = workspaceFolders[0].uri.fsPath;
            console.log('Sending workspace context via REST API:', workspacePath);
            
            try {
                const success = await this._apiService.setWorkspacePath(workspacePath);
                if (success) {
                    console.log('Workspace context sent successfully');
                } else {
                    console.error('Failed to send workspace context via REST API');
                    vscode.window.showErrorMessage('Failed to set workspace context via REST API');
                }
            } catch (error) {
                console.error('Failed to send workspace context:', error);
                vscode.window.showErrorMessage(`Failed to set workspace context: ${error}`);
            }
        } else {
            console.log('No workspace folders available');
            vscode.window.showWarningMessage('No workspace is currently open. Please open a workspace to use the AI agents.');
        }
    }
    
    public async onWorkspaceChanged() {
        console.log('Workspace changed, updating context');
        // Update API service in case backend URL configuration changed
        this._apiService = new WorkspaceApiService(WorkspaceApiService.getConfiguredBaseUrl());
        await this.sendWorkspaceContext();
    }
    
    public clearConnection() {
        // No connection to clear with REST API
    }
}