import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CakeshopService } from '@app/cakeshop.service';

@Component({
  selector: 'app-order-management',
  standalone: false,
  templateUrl: './order-management.component.html',
  styleUrl: './order-management.component.css'
})
export class OrderManagementComponent implements OnInit {
  orders: any[] = [];

  constructor(private service: CakeshopService) {}

  ngOnInit(): void {
    this.fetchOrders();
  }

  fetchOrders() {
    this.service.getAllOrders().subscribe({
      next: res => {
        if (res.status === 'success') {
          this.orders = res.data;
        }
      },
      error: err => console.error('Error loading orders', err)
    });
  }

  updateStatus(order: any) {
    this.service.updateOrderStatus(order.OrderID, order.Status).subscribe({
      next: res => {
        alert('Cập nhật thành công!');
        this.fetchOrders(); // Refresh
      },
      error: err => {
        alert('Lỗi cập nhật trạng thái.');
        console.error(err);
      }
    });
  }
}