export function showErrorMessage(message) {
    console.log(message);
    const canvasContainer = document.getElementById('out');
    if (canvasContainer) {
        canvasContainer.style.display = 'none';
    }

    // Create and show a static error container
    const errorDiv = document.createElement('div');
    errorDiv.style.cssText = 'padding: 40px; font-family: sans-serif; color: #721c24; background-color: #f8d7da; height: 100vh;';
    errorDiv.innerHTML = `
            <h1>Application Failed to Start</h1>
            <p>A critical error occurred while initializing the Avalonia WebAssembly runtime:</p>
            <pre style="background: #fff; padding: 15px; border: 1px solid #f5c6cb;">${message}</pre>
            <button onclick="location.reload()" style="padding: 10px 20px; cursor: pointer;">Reload Page</button>
        `;
    document.body.appendChild(errorDiv);
}