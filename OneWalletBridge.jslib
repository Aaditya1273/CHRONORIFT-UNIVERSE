/**
 * OneWallet Bridge for Unity WebGL
 * Connects Unity game to OneWallet browser extension
 */

mergeInto(LibraryManager.library, {
    
    // Check if OneWallet is available
    IsOneWalletAvailable: function() {
        return typeof window.oneWallet !== 'undefined';
    },

    // Get pre-connected wallet address from Next.js
    GetPreConnectedWallet: function() {
        // Check if wallet address was set by Next.js
        if (window.preConnectedWalletAddress) {
            var address = window.preConnectedWalletAddress;
            var bufferSize = lengthBytesUTF8(address) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(address, buffer, bufferSize);
            return buffer;
        }
        return null;
    },

    // Connect to OneWallet
    ConnectOneWallet: function(gameObjectName, callbackMethod, errorMethod) {
        var objName = UTF8ToString(gameObjectName);
        var callback = UTF8ToString(callbackMethod);
        var error = UTF8ToString(errorMethod);

        if (typeof window.oneWallet === 'undefined') {
            SendMessage(objName, error, 'OneWallet not found. Please install OneWallet extension.');
            return;
        }

        // Request wallet connection
        window.oneWallet.connect()
            .then(function(accounts) {
                if (accounts && accounts.length > 0) {
                    var address = accounts[0];
                    console.log('OneWallet connected:', address);
                    SendMessage(objName, callback, address);
                } else {
                    SendMessage(objName, error, 'No accounts found');
                }
            })
            .catch(function(err) {
                console.error('OneWallet connection error:', err);
                SendMessage(objName, error, err.message || 'Connection failed');
            });
    },

    // Get current wallet address
    GetOneWalletAddress: function(gameObjectName, callbackMethod) {
        var objName = UTF8ToString(gameObjectName);
        var callback = UTF8ToString(callbackMethod);

        if (typeof window.oneWallet === 'undefined') {
            return;
        }

        window.oneWallet.getAccounts()
            .then(function(accounts) {
                if (accounts && accounts.length > 0) {
                    SendMessage(objName, callback, accounts[0]);
                }
            })
            .catch(function(err) {
                console.error('Error getting address:', err);
            });
    },

    // Sign and send transaction to OneChain
    SendOneChainTransaction: function(gameObjectName, txData, callbackMethod, errorMethod) {
        var objName = UTF8ToString(gameObjectName);
        var transaction = UTF8ToString(txData);
        var callback = UTF8ToString(callbackMethod);
        var error = UTF8ToString(errorMethod);

        if (typeof window.oneWallet === 'undefined') {
            SendMessage(objName, error, 'OneWallet not available');
            return;
        }

        try {
            var txObject = JSON.parse(transaction);
            
            window.oneWallet.signAndExecuteTransactionBlock({
                transactionBlock: txObject,
                options: {
                    showEffects: true,
                    showEvents: true,
                }
            })
            .then(function(result) {
                console.log('Transaction successful:', result);
                var resultJson = JSON.stringify(result);
                SendMessage(objName, callback, resultJson);
            })
            .catch(function(err) {
                console.error('Transaction error:', err);
                SendMessage(objName, error, err.message || 'Transaction failed');
            });
        } catch (e) {
            SendMessage(objName, error, 'Invalid transaction data: ' + e.message);
        }
    },

    // Query OneChain (read-only)
    QueryOneChain: function(gameObjectName, queryData, callbackMethod, errorMethod) {
        var objName = UTF8ToString(gameObjectName);
        var query = UTF8ToString(queryData);
        var callback = UTF8ToString(callbackMethod);
        var error = UTF8ToString(errorMethod);

        try {
            var queryObject = JSON.parse(query);
            
            // Use OneChain RPC endpoint
            fetch('https://rpc-testnet.onelabs.cc:443', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    jsonrpc: '2.0',
                    id: 1,
                    method: queryObject.method,
                    params: queryObject.params
                })
            })
            .then(function(response) {
                return response.json();
            })
            .then(function(data) {
                var resultJson = JSON.stringify(data.result);
                SendMessage(objName, callback, resultJson);
            })
            .catch(function(err) {
                console.error('Query error:', err);
                SendMessage(objName, error, err.message || 'Query failed');
            });
        } catch (e) {
            SendMessage(objName, error, 'Invalid query data: ' + e.message);
        }
    }
});
