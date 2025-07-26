# VSCode AI Coding Assistant Extension

A VSCode extension that provides AI-powered coding assistance with workspace synchronization. This extension hosts the AI Coding Assistant UI and uses REST APIs for workspace synchronization.

## Features

- **Workspace Synchronization**: Automatically syncs workspace path and active document with the backend server
- **File Change Monitoring**: Tracks file changes in the workspace (displayed in the webview)
- **AI Coding Assistant UI**: Embeds the AI Coding Assistant web application in the VSCode side panel
- **Simple Integration**: The extension only handles workspace context - all chat and AI functionality is provided by the hosted UI

## Configuration

### Backend URL

You can configure the backend server URL in VSCode settings:

1. Open VSCode Settings (Ctrl+,)
2. Search for "AI Coding Assistant"
3. Set the "Backend Url" setting to your server URL (default: `http://localhost:50535`)

Alternatively, you can set it in your `settings.json`:

```json
{
  "aiCodingAssistant.backendUrl": "http://localhost:50535"
}
```

## REST API Endpoints

The extension communicates with the backend server using the following REST API endpoints:

- `POST /api/workspace/path` - Sets the workspace path
- `POST /api/workspace/activedocument` - Sets the active document path

### Request Format

Both endpoints expect a JSON payload with the following structure:

```json
{
  "path": "/full/path/to/workspace/or/document"
}
```


## Development

### Prerequisites

- Node.js and npm
- TypeScript
- VSCode Extension Development Host

### Building

```bash
npm install
npm run compile
```

### Packaging

```bash
npm run package
```

### Testing

```bash
npm run test
```

## Usage

1. Install the extension in VSCode
2. Configure the backend URL in settings
3. Open a workspace
4. The extension will automatically sync workspace information with the backend
5. Use the Blazor Explorer view in the activity bar to see workspace information

## Architecture

```
┌─────────────────────┐    HTTP POST    ┌─────────────────────┐
│                     │ ──────────────► │                     │
│  VSCode Extension   │                 │  Backend Server     │
│                     │                 │                     │
│  - WorkspaceApiService                 │  - WorkspaceController │
│  - File monitoring  │                 │  - REST API endpoints │
│  - UI hosting (iframe) │               │  - AI Coding Assistant UI │
│                     │                 │  - AI chat functionality │
└─────────────────────┘                 └─────────────────────┘
```

