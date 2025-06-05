import { Component, OnInit } from '@angular/core';
import { CakeshopService, ChangePasswordDto } from '../../cakeshop.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-user-profile',
  standalone: false,
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent implements OnInit {
  user: any = {};
  userId: number = 0;

  oldPassword = '';
  newPassword = '';

  constructor(private service: CakeshopService,private router: Router) {}

  ngOnInit() {
    this.userId = Number(localStorage.getItem('userId'));
    this.service.getCustomersById(this.userId).subscribe((res: any) => {
      this.user = res[0]; // đảm bảo là object chứ không phải array rỗng
      console.log("User nhận được:", this.user);
    });
  }

  updateCustomerInfo() {
    const updateData = {
      FullName: this.user.FullName,
      Phone: this.user.Phone,
      Address: this.user.Address,
      Email: this.user.Email
    };
  
    console.log("Payload gửi lên:", updateData);
  
    this.service.updateUserProfile(this.userId, updateData).subscribe({
      next: (res) => alert("Cập nhật thành công"),
      error: (err) => {
        console.error(" Lỗi cập nhật", err);
        if (err.error?.message) {
          alert(` ${err.error.message}`);
        } else {
          alert(" Đã xảy ra lỗi khi cập nhật.");
        }
      }
    });
  }
  

  changePassword() {
    const data: ChangePasswordDto = {
      userId: this.userId,
      oldPassword: this.oldPassword,
      newPassword: this.newPassword
    };

    this.service.changePassword(data).subscribe({
      next: (res) => {
        alert('Đổi mật khẩu thành công!');
      },
      error: (err) => {
        alert('Lỗi: ' + err.error?.message || 'Không thể đổi mật khẩu');
      }
    });
  }
  goBack() {
    this.router.navigate(['/user/home/menu']); // Điều hướng về trang danh sách sản phẩm
  }
}

