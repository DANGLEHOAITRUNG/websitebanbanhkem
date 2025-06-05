import { Component, OnInit } from '@angular/core';
import { CakeshopService } from '../../cakeshop.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-product-add',
  standalone: false,
  templateUrl: './product-add.component.html',
  styleUrl: './product-add.component.css'
})
export class ProductAddComponent implements OnInit {
  // Đối tượng product để lưu thông tin sản phẩm từ form
  product: any = {};
  categories: any[] = [];

  // Mảng chứa các lỗi khi thêm sản phẩm
  errors: string[] = [];

  constructor(private service: CakeshopService, private router: Router) {}

  ngOnInit() {
    // Load danh mục khi khởi động
    this.service.getAllCategories().subscribe(
      (response) => {
        if (Array.isArray(response)) {
          this.categories = response;
        } else {
          console.error('Expected an array but got:', response);
        }
      },
      (error) => console.error('Error loading categories:', error)
    );
  }

  // Hàm thêm sản phẩm
  addProduct() {
    // Xóa các lỗi cũ trước khi kiểm tra lại
    this.errors = [];

    // Kiểm tra hợp lệ cho các trường thông tin
    if (!this.product.ProductName || this.product.ProductName.trim() === '') {
      this.errors.push('Tên sản phẩm không được để trống');
    }
    if (this.product.Price == null || this.product.Price < 0) {
      this.errors.push('Giá sản phẩm phải lớn hơn hoặc bằng 0');
    }
    if (this.product.Quantity == null || this.product.Quantity < 0) {
      this.errors.push('Số lượng sản phẩm phải lớn hơn hoặc bằng 0');
    }
    if (!this.product.ImageUrl || !this.isValidUrl(this.product.ImageUrl)) {
      this.errors.push('URL hình ảnh không hợp lệ');
    }
    if (!this.product.CategoryID) {
      this.errors.push('Vui lòng chọn danh mục');
    }

    // Nếu có lỗi, không gửi yêu cầu lên server
    if (this.errors.length > 0) {
      return;
    }

    // Gọi service để thêm sản phẩm
    this.service.addProduct(this.product).subscribe(
      () => {
        alert('Thêm sản phẩm thành công!');
        setTimeout(() => this.router.navigate(['/admin/admin-home/product-management']), 3000);
      },
      (error) => {
        if (error.status === 400 && error.error.errors) {
          this.errors = error.error.errors;
        } else {
          this.errors.push('Đã có lỗi xảy ra. Vui lòng thử lại sau!');
        }
      }
    );
  }

  // Hàm kiểm tra URL có hợp lệ không
  private isValidUrl(url: string): boolean {
    const urlPattern = /^(http|https):\/\/[^ "']+$/;
    return urlPattern.test(url);
  }
}