export async function triggerGoogleFedCM(clientId) {
    try {
        const credential = await navigator.credentials.get({
            identity: {
                providers: [{
                    configURL: 'https://accounts.google.com/gsi/fedcm.json',
                    clientId: clientId,
                    params: {
                        nonce: crypto.randomUUID(),
                        scope: 'openid email profile',
                        response_type: 'code'
                    }
                }],
                // Required for FedCM to function for Google Sign-in
                mode: 'passive'
            }
        });

        // This token is the JSON Web Token (JWT) sent by Google
        return credential.token;
    } catch (error) {
        console.error("FedCM sign-in failed:", error);
        return null;
    }
}
export async function handleCredentialResponse(credential) {
    const email = await globalThis.SWSMonitor.Browser.Program.HandleGoogleCredentialAsync(credential);
    return email;
}
