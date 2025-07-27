import * as vscode from 'vscode';

export interface SetPathRequest {
    path: string;
}

export class WorkspaceApiService {
    private baseUrl: string;

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    async setWorkspacePath(path: string): Promise<boolean> {
        try {
            const response = await fetch(`${this.baseUrl}/api/workspace/path`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ path } as SetPathRequest),
            });

            if (!response.ok) {
                console.error(`Failed to set workspace path: ${response.status} ${response.statusText}`);
                return false;
            }

            return true;
        } catch (error) {
            console.error('Error setting workspace path:', error);
            return false;
        }
    }

    async setActiveDocument(path: string): Promise<boolean> {
        try {
            const response = await fetch(`${this.baseUrl}/api/workspace/activedocument`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ path } as SetPathRequest),
            });

            if (!response.ok) {
                console.error(`Failed to set active document: ${response.status} ${response.statusText}`);
                return false;
            }

            return true;
        } catch (error) {
            console.error('Error setting active document:', error);
            return false;
        }
    }

    static getConfiguredBaseUrl(): string {
        const config = vscode.workspace.getConfiguration('aiCodingAssistant');
        let baseUrl = config.get<string>('backendUrl');
        
        if (!baseUrl) {
            baseUrl = 'http://localhost:50535';
        }
        
        // Remove trailing slash if present
        return baseUrl.replace(/\/$/, '');
    }

    static isRunningInDevContainer(): boolean {
        return process.env.REMOTE_CONTAINERS === 'true' ||
               process.env.CODESPACES === 'true' ||
               process.env.VSCODE_REMOTE_CONTAINERS_SESSION !== undefined;
    }
}