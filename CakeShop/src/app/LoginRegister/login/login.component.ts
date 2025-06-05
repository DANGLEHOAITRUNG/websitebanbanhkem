import { Component, OnInit } from '@angular/core';
import { CakeshopService } from '../../cakeshop.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  userName: string = '';
  passWord: string = '';

  constructor(private service: CakeshopService, private router: Router) {}

  ngOnInit(): void {}

  dangNhap() {
    if (!this.userName.trim() || !this.passWord.trim()) {
      alert('Vui lòng điền đầy đủ tên đăng nhập và mật khẩu!');
      return;
    }

    const loginPayload = {
      Username: this.userName,
      Password: this.passWord
    };

    console.log(' Dữ liệu gửi đi:', loginPayload);

    this.service.dangNhap(loginPayload).subscribe(
      (response: any) => {
        console.log(' Dữ liệu API trả về:', response);

        if (response.status === 'success' && response.data) {
          const userData = response.data;

          console.log(' Dữ liệu người dùng:', userData);

          const userId = userData.userID ?? userData.UserID;
          const roleId = userData.roleID ?? userData.RoleID;
          const username = userData.userName ?? userData.Username;

          if (userId && roleId !== undefined) {
            localStorage.setItem('userId', userId.toString());
            localStorage.setItem('roleID', roleId.toString());
            localStorage.setItem('userName', username ?? '');

            this.service.updateLoginStatus(true);

            if (parseInt(roleId) === 2) {
              this.router.navigate(['/admin/admin-home']);
            } else {
              this.router.navigate(['/user/home']);
            }
          } else {
            alert(' Thiếu thông tin người dùng trong phản hồi!');
            console.error('userID hoặc roleID bị thiếu trong phản hồi:', response);
          }
        } else {
          alert(" Tên đăng nhập hoặc mật khẩu không đúng.");
        }
      },
      (error: any) => {
        console.error(' Lỗi đăng nhập:', error);
        alert("Có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại sau.");
      }
    );
  }
}