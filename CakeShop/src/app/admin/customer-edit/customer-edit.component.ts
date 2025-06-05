import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CakeshopService } from '../../cakeshop.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-customer-edit',
  standalone: false,
  templateUrl: './customer-edit.component.html',
  styleUrl: './customer-edit.component.css'
})
export class CustomerEditComponent implements OnInit {
  userId: number = 0;
  user: any = {};
  errors: string[] = [];

  constructor(
    private route: ActivatedRoute,
    private service: CakeshopService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCustomerData();
  }

  loadCustomerData() {
    this.service.getCustomersById(this.userId).subscribe(
      (res: any) => {
        if (res.length > 0) {
          this.user = res[0];
        } else {
          this.errors.push('Không tìm thấy người dùng.');
        }
      },
      error => {
        this.errors.push('Lỗi khi lấy thông tin người dùng.');
      }
    );
  }

  updateUser() {
    this.errors = [];

    if (!this.user.Username || this.user.Username.trim() === '') {
      this.errors.push('Tên đăng nhập không được để trống');
    }
    if (!this.user.Email || !this.isValidEmail(this.user.Email)) {
      this.errors.push('Email không hợp lệ');
    }
    if (!this.user.Phone || !this.isValidPhone(this.user.Phone)) {
      this.errors.push('Số điện thoại không hợp lệ');
    }

    if (this.errors.length > 0) {
      return;
    }

    this.service.updateCustomer(this.userId, this.user).subscribe(
      () => {
        alert('Cập nhật thông tin thành công!');
        this.router.navigate(['/admin/admin-home/customer-management']);
      },
      error => {
        if (error.status === 400 && error.error.errors) {
          this.errors = error.error.errors;
        } else {
          this.errors.push('Đã có lỗi xảy ra. Vui lòng thử lại sau!');
        }
      }
    );
  }

  private isValidEmail(email: string): boolean {
    const emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    return emailPattern.test(email);
  }

  private isValidPhone(phone: string): boolean {
    const phonePattern = /^[0-9]{10}$/;
    return phonePattern.test(phone);
  }
}