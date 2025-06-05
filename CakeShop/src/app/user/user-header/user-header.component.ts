import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CakeshopService } from '@app/cakeshop.service';

@Component({
  selector: 'app-user-header',
  standalone: false,
  templateUrl: './user-header.component.html',
  styleUrl: './user-header.component.css'
})
export class UserHeaderComponent implements OnInit {
  isLoggedIn = false;
  userName: string = '';
  userId: number = 0;
  cartCount: number = 0;
  searchTerm: string = '';

  constructor(private router: Router, private service: CakeshopService) {}

  ngOnInit(): void {
    const userIdStr = localStorage.getItem('userId');
    const userNameStr = localStorage.getItem('userName');

    if (userIdStr && userNameStr) {
      this.isLoggedIn = true;
      this.userName = userNameStr;
      this.userId = parseInt(userIdStr, 10);
    }

    // Gọi backend cập nhật số lượng giỏ hàng
    this.service.refreshCartCount(this.userId);

    // Lắng nghe biến đếm giỏ hàng
    this.service.cartCount$.subscribe(count => {
      this.cartCount = count;
    });
  }

  goBack() {
    this.router.navigate(['/user/home/menu']);
  }
  
  logout() {
    localStorage.clear();
    this.isLoggedIn = false;
    this.router.navigate(['/login']);
  }
}