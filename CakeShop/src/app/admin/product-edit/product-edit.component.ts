import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CakeshopService } from '@app/cakeshop.service';

@Component({
  selector: 'app-product-edit',
  standalone: false,
  templateUrl: './product-edit.component.html',
  styleUrl: './product-edit.component.css'
})
export class ProductEditComponent implements OnInit {
  product: any = {};  // Sản phẩm cần chỉnh sửa
  categories: any[] = []; // Danh sách danh mục
  errors: string[] = []; // Lưu lỗi khi cập nhật sản phẩm

  constructor(
    private service: CakeshopService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    // Lấy ID sản phẩm từ URL
    const productId = this.route.snapshot.paramMap.get('id');
    if (productId) {
      this.getProductById(parseInt(productId));
    }

    // Lấy danh sách danh mục để hiển thị trong dropdown
    this.service.getAllCategories().subscribe(
      (response) => this.categories = response,
      (error) => console.error('Lỗi khi tải danh mục:', error)
    );
  }

  // Lấy thông tin sản phẩm theo ID
  getProductById(id: number) {
    this.service.getProductById(id).subscribe(
      (response) => this.product = response,
      (error) => console.error('Lỗi khi tải sản phẩm:', error)
    );
  }

  // Cập nhật sản phẩm
  updateProduct() {
    this.errors = [];
  
    // Kiểm tra hợp lệ dữ liệu
    if (!this.product.ProductName || this.product.ProductName.trim() === '') {
      this.errors.push('Tên sản phẩm không được để trống');
    }
    if (this.product.Price == null || this.product.Price < 0) {
      this.errors.push('Giá sản phẩm phải lớn hơn hoặc bằng 0');
    }
    if (this.product.Quantity == null || this.product.Quantity < 0) {
      this.errors.push('Số lượng sản phẩm phải lớn hơn hoặc bằng 0');
    }
  
    // Kiểm tra URL hình ảnh
    console.log('URL hình ảnh nhập vào: ', this.product.ImageURL);
    if (!this.product.ImageURL || !this.isValidUrl(this.product.ImageURL)) {
      this.errors.push('URL hình ảnh không hợp lệ');
    }
  
    if (!this.product.CategoryID) {
      this.errors.push('Vui lòng chọn danh mục');
    }
  
    // Nếu có lỗi, không gửi yêu cầu cập nhật
    if (this.errors.length > 0) return;
  
    // Gửi yêu cầu cập nhật sản phẩm
    this.service.updateProduct(this.product.ProductID, this.product).subscribe(
      () => {
        alert('Cập nhật sản phẩm thành công!');
        this.router.navigate(['/admin/admin-home/product-management']);
      },
      (error) => {
        if (error.status === 400 && error.error.errors) {
          this.errors = error.error.errors;
        } else {
          this.errors.push('Có lỗi xảy ra khi cập nhật sản phẩm!');
        }
      }
    );
  }
  
  // Kiểm tra URL hình ảnh có hợp lệ không
  private isValidUrl(url: string): boolean {
    // Biểu thức chính quy để kiểm tra các URL hợp lệ cho hình ảnh, bao gồm Bing Image URLs
    const urlPattern = /^(https?:\/\/)?([a-z0-9-]+\.)+[a-z]{2,6}(\/[^\s]*)?\.(jpg|jpeg|png|gif|webp)$/i;
    // Chấp nhận URL từ Bing (với điều kiện có kết thúc hình ảnh)
    if (url.includes("bing.com")) {
      return true;  // Chấp nhận URL từ Bing vì nó dẫn đến hình ảnh mặc dù không phải tệp ảnh trực tiếp
    }
    return urlPattern.test(url);  // Kiểm tra URL có hợp lệ không
  }
  
  
}