# SWSMonitor

A cross-platform monitoring application built with [Avalonia UI](https://avaloniaui.net/), with a browser-based WASM version hosted on GitHub Pages.

## Live Demo

The WASM build is automatically deployed to GitHub Pages on every push to `main`:
**https://dragnldy.github.io/SWSMonitor/**

## Projects

| Project | Description |
|---|---|
| `SWSMonitor` | Shared application logic and UI (Avalonia) |
| `SWSMonitor.Browser` | WebAssembly (WASM) browser target |
| `SWSMonitor.Desktop` | Desktop target (Windows/Linux/macOS) |
| `SWSMonitor.Android` | Android target |
| `SWSMonitor.iOS` | iOS target |

## Deployment

The GitHub Actions workflow (`.github/workflows/deploy-pages.yml`) automatically:
1. Builds the `SWSMonitor.Browser` project for WebAssembly using `dotnet publish`
2. Deploys the output to GitHub Pages

### Enable GitHub Pages

To activate GitHub Pages for this repository:
1. Go to **Settings → Pages**
2. Under **Build and deployment**, set the source to **GitHub Actions**
