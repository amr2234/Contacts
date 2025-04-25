class SignalRHandler {
    constructor() {
        this.connection = null;
        this.onLockUpdate = null;
        this.onContactUpdate = null;
        this.onError = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
    }
    
    async start() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/contactHub")
            .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
            .build();
        
        this.setupEventHandlers();
        
        await this.connection.start();
        this.isConnected = true;
    }
    
    setupEventHandlers() {
        const events = {
            "ContactLocked": (contactId) => this.onLockUpdate?.(contactId, true, this.connection.connectionId),
            "ContactUnlocked": (contactId) => this.onLockUpdate?.(contactId, false, null),
            "Error": (message) => this.onError?.(message),
            "ContactUpdated": (contactId, field, value) => this.onContactUpdate?.(contactId, field, value),
        };
        
        Object.entries(events).forEach(([event, handler]) => {
            this.connection.on(event, handler);
        });
        
        this.connection.onreconnecting(() => {
            this.isConnected = false;
        });
        
        this.connection.onreconnected(() => {
            this.isConnected = true;
            this.reconnectAttempts = 0;
        });
        
        this.connection.onclose(() => {
            this.isConnected = false;
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                this.reconnectAttempts++;
                setTimeout(() => this.start(), 2000 * this.reconnectAttempts);
            }
        });
    }
    
    async invokeMethod(method, ...args) {
        if (!this.connection) {
            throw new Error('SignalR connection not established');
        }
        if (!this.isConnected) {
            throw new Error('SignalR connection is not active');
        }
        await this.connection.invoke(method, ...args);
    }
    
    async lockContact(contactId) {
        await this.invokeMethod('LockContact', contactId);
    }
    
    async unlockContact(contactId) {
        await this.invokeMethod('UnlockContact', contactId);
    }
    
    async updateContact(contactId, field, value) {
        await this.invokeMethod('UpdateContact', contactId, field, value);
    }

}