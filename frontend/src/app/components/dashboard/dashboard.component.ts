import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, signal } from '@angular/core';
import { Transaction, TransactionService } from '../../services/transaction-service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, CurrencyPipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {
  transactions = signal<Transaction[]>([]);
  newTransaction: Transaction = {
    customerName: '',
    productName: '',
    amount: 0,
  };

  constructor(private transactionService: TransactionService){}

  ngOnInit() {
    this.transactionService.loadTransactions();
    this.transactions = this.transactionService.transactions;
    this.newTransaction = {
      customerName: '',
      productName: '',
      amount: 0,
    };
  }  

  createTransaction() {
    this.transactionService.createTransaction(this.newTransaction).subscribe(() => {
      this.newTransaction = {
        customerName: '',
        productName: '',
        amount: 0,
      };
    });
  }

}
