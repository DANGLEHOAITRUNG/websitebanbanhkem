import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-home',
  standalone: false,
  templateUrl: './admin-home.component.html',
  styleUrl: './admin-home.component.css'
})
export class AdminHomeComponent {
  constructor(private router: Router) {}

  navigateTo(path: string): void {
    this.router.navigate([`/admin/${path}`]);
  }
  
  logout() {
    alert('Bạn đã đăng xuất!');
    this.router.navigate(['/login']);
  }
}
