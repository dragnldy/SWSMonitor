// ============================================================================
// Browser Storage JavaScript Interop Functions
// Separate from main.js to avoid circular dependency
// ============================================================================

// Export individual functions (not as object) for JSImport compatibility
// All functions return Promises to match C# Task return types
export function setItem(key, value) {
    return Promise.resolve().then(() => {
        try {
            window.localStorage.setItem(key, value);
            console.log(`localStorage.setItem: ${key}`);
        } catch (error) {
            console.error('Error setting localStorage item:', error);
            throw error;
        }
    });
}

export function getItem(key) {
    return Promise.resolve().then(() => {
        try {
            const value = window.localStorage.getItem(key);
            console.log(`localStorage.getItem: ${key} = ${value ? 'found' : 'null'}`);
            return value;
        } catch (error) {
            console.error('Error getting localStorage item:', error);
            return null;
        }
    });
}

export function removeItem(key) {
    return Promise.resolve().then(() => {
        try {
            window.localStorage.removeItem(key);
            console.log(`localStorage.removeItem: ${key}`);
        } catch (error) {
            console.error('Error removing localStorage item:', error);
            throw error;
        }
    });
}

// Clear all items with a specific prefix
export function clearStorageWithPrefix(prefix) {
    return Promise.resolve().then(() => {
        try {
            const keysToRemove = [];
            for (let i = 0; i < window.localStorage.length; i++) {
                const key = window.localStorage.key(i);
                if (key && key.startsWith(prefix)) {
                    keysToRemove.push(key);
                }
            }

            keysToRemove.forEach(key => {
                window.localStorage.removeItem(key);
            });

            console.log(`Cleared ${keysToRemove.length} items with prefix: ${prefix}`);
        } catch (error) {
            console.error('Error clearing storage with prefix:', error);
            throw error;
        }
    });
}
