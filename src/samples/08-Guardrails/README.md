# 08 — Guardrails & AI Safety

Implement input and output safety validation using guard agents.

## What You'll Learn

- Building a two-layer safety pipeline (input guard + output guard)
- Using a separate agent as a content safety reviewer
- Blocking harmful inputs before they reach the main agent
- Blocking unsafe outputs before they reach the user

## Architecture

```
User → [InputGuard] ──→ [CodingAgent] ──→ [SafetyGuard] ──→ User
         ↓ BLOCK           process           ↓ BLOCK
```

## Key Pattern

The guard agent receives both the user request and the draft response, then classifies it as `SAFE` or `BLOCKED`:

```
SAFE: The response provides standard file I/O code with no security issues.
BLOCKED: The response contains SQL injection-vulnerable code.
```

## Run

```bash
cp .env.example .env
dotnet run
```

Try asking a normal coding question, then try to get it to produce something unsafe.

## Corresponding Notebook

📓 [8-Guardrails-and-AI-Safety.ipynb](../../../notebooks/8-Guardrails-and-AI-Safety.ipynb)
