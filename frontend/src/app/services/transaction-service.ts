import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

export interface Transaction {
  id?: string;
  transactionId?: string;
  productName: string;
  customerName: string;
  amount: number;
  status? : 'Pending' | 'Processing' | 'Completed' | 'Failed';
  createdAt?: Date;
  processedAt?: Date;
  errorMessage?: string;
}

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
  private apiUrl = 'https://localhost:7003/api/transactions';
  private hubConnection!: HubConnection;

  transactions = signal<Transaction[]>([]);
  
  constructor(private http: HttpClient){
    this.setupSignalR();
  }

  async setupSignalR() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('https://localhost:7003/hubs/transactions')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveTransactionUpdate', (transaction: Transaction) => {
      this.updateTransaction(transaction); 
    });

    try {
      await this.hubConnection.start();
      console.log('SignalR Connected');
    } catch (err) {
      console.error('SignalR Error:', err);
    }
  }

  loadTransactions() {
    this.http.get<Transaction[]>(this.apiUrl).subscribe({
      next: (data: Transaction[]) => this.transactions.set(data),
      error: (err: any) => console.error('Error loading modelos:', err)
    });
  }

  createTransaction(transaction: Transaction) {
    return this.http.post<Transaction>(this.apiUrl, transaction);
  }

  updateTransaction(transaction: Transaction) {
    this.transactions.update(current => {
      const index = current.findIndex(t => t.transactionId === transaction.transactionId);

      if (index >= 0) {
        const updated = [...current];
        updated[index] = transaction;
        return updated;
      } else {
        return [transaction, ...current];
      }
    });
  }
}
