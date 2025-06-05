import { Component } from '@angular/core';
import { CakeshopService } from '../../cakeshop.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-customer-add',
  standalone: false,
  templateUrl: './customer-add.component.html',
  styleUrl: './customer-add.component.css'
})
export class CustomerAddComponent {
  user: any = {};
  errors: string[] = [];

  constructor(private service: CakeshopService, private router: Router) { }

  addUser() {
    // Clear previous errors
    this.errors = [];

    // Validate input fields
    if (!this.user.Username || this.user.Username.trim() === '') {
      this.errors.push('Tên đăng nhập không được để trống');
    }
    if (!this.user.Email || !this.isValidEmail(this.user.Email)) {
      this.errors.push('Email không hợp lệ');
    }
    if (!this.user.Phone || !this.isValidPhone(this.user.Phone)) {
      this.errors.push('Số điện thoại không hợp lệ');
    }

    // If there are validation errors, do not send request
    if (this.errors.length > 0) {
      return;
    }

    // Call the service to add customer
    this.service.addCustomer(this.user).subscribe(
      () => {
        alert('Thêm tài khoản thành công!');
        setTimeout(() => {
          this.router.navigate(['/admin/admin-home/customer-management']);
        }, 3000);
      },
      (error) => {
        if (error.status === 400 && error.error.errors) {
          // If server returns errors (like duplicate username, email, or phone)
          this.errors = error.error.errors;
        } else {
          this.errors.push('Đã có lỗi xảy ra. Vui lòng thử lại sau!');
        }
      }
    );
  }

  // Email validation function
  private isValidEmail(email: string): boolean {
    const emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    return emailPattern.test(email);
  }

  // Phone number validation function (basic validation for 10 digits)
  private isValidPhone(phone: string): boolean {
    const phonePattern = /^[0-9]{10}$/;
    return phonePattern.test(phone);
  }
}