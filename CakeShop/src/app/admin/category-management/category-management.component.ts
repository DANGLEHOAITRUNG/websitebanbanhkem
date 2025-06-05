import { Component, OnInit } from '@angular/core';
import { CakeshopService } from '../../cakeshop.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-category-management',
  standalone: false,
  templateUrl: './category-management.component.html',
  styleUrl: './category-management.component.css'
})
export class CategoryManagementComponent implements OnInit {
  categories: any[] = [];
  newCategory: any = { categoryName: '' };
  selectedCategory: any = null;

  constructor(private service: CakeshopService) { }

  ngOnInit(): void {
    this.loadCategories();
  }

  // Lấy danh sách danh mục
  loadCategories(): void {
    this.service.getAllCategories().subscribe(data => {
      this.categories = data;
    });
  }

  // Thêm danh mục
  addCategory(): void {
    if (!this.newCategory.categoryName.trim()) {
      alert('Tên danh mục không được để trống!');
      return;
    }

    this.service.addCategory(this.newCategory).subscribe(() => {
      alert('Thêm danh mục thành công!');
      this.newCategory.categoryName = '';
      this.loadCategories();
    });
  }

  // Chọn danh mục để cập nhật
  selectCategory(category: any): void {
    this.selectedCategory = { ...category };
  }

  // Cập nhật danh mục
  updateCategory(): void {
    if (!this.selectedCategory.CategoryName.trim()) {
      alert('Tên danh mục không được để trống!');
      return;
    }

    this.service.updateCategory(this.selectedCategory.CategoryID, this.selectedCategory).subscribe(() => {
      alert('Cập nhật thành công!');
      this.selectedCategory = null;
      this.loadCategories();
    });
  }

  // Xóa danh mục
  deleteCategory(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa danh mục này?')) {
      this.service.deleteCategory(id).subscribe(() => {
        alert('Xóa thành công!');
        this.loadCategories();
      });
    }
  }
}