import { Component, OnInit } from '@angular/core';
import { CakeshopService } from '../../cakeshop.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-customer-management',
  standalone: false,
  templateUrl: './customer-management.component.html',
  styleUrl: './customer-management.component.css'
})
export class CustomerManagementComponent implements OnInit {
  Customer: any[] = [];

  constructor(private service: CakeshopService, private router: Router) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers() {
    this.service.getAllCustomers().subscribe(
      (data: any) => {
        if (Array.isArray(data)) {
          this.Customer = data;
        } else {
          console.error('Dữ liệu trả về không phải là mảng:', data);
        }
      },
      (error) => {
        console.error('Lỗi khi tải danh sách người dùng:', error);
      }
    );
  }
  goBack() {
    setTimeout(() => {
      this.router.navigate(['/admin/admin-home/customer-management']);
    }, 3000);
  }

  deleteUser(id: number) {
    if (confirm('Bạn có chắc muốn xóa người dùng này không?')) {
      this.service.deleteCustomer(id).subscribe(() => {
        alert('Xóa thành công!');
        this.loadUsers();
      }, (error) => {
        console.error('Lỗi khi xóa người dùng:', error);
      });
    }
  }
}