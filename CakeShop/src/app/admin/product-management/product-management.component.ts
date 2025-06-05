import { Component, OnInit } from '@angular/core';
import { CakeshopService } from '../../cakeshop.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-product-management',
  standalone: false,
  templateUrl: './product-management.component.html',
  styleUrl: './product-management.component.css'
})
export class ProductManagementComponent implements OnInit {
  products: any[] = [];

  constructor(private service: CakeshopService, private router: Router) { }

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.service.getAllProducts().subscribe((data) => {
      this.products = data;
    });
  }

  deleteProduct(id: number) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này không?')) {
      this.service.deleteProduct(id).subscribe(() => {
        alert('Xóa sản phẩm thành công!');
        this.loadProducts();
      });
    }
  }

  goToAdd() {
    this.router.navigate(['/add']);
  }

  goToEdit(id: number) {
    this.router.navigate(['/edit', id]);
  }
}