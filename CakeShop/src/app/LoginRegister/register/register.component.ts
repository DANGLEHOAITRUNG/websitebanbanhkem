import { Component, OnInit } from '@angular/core';
import { CakeshopService } from '../../cakeshop.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  userName: string = ''; 
  email: string = '';
  phone: string = '';
  address: string = '';
  isAgreed: boolean = false;
  passWord: string = '';
  confirmPassword: string = '';

  dky: any = {
    userName: "",
    email: "",
    phone: "",
    address: "",
    isAgreed: "",
    passWord: "",
    confirmPassword: ""
  };

  constructor(private service: CakeshopService) {}

  ngOnInit(): void {}

  dangKy() {
      // Kiểm tra nếu bất kỳ trường nào bị bỏ trống
    if (!this.userName.trim() || !this.email.trim() || !this.phone.trim() || 
      !this.address.trim() || !this.passWord.trim() || !this.confirmPassword.trim()) {
      alert('Vui lòng điền đầy đủ tất cả các trường!');
      return;
 }
     // Kiểm tra username phải chứa cả chữ và số
     const hasLetter = /[a-zA-Z]/.test(this.userName); // Kiểm tra chữ
     const hasNumber = /\d/.test(this.userName); // Kiểm tra số
     if (!(hasLetter && hasNumber)) {
       alert('Tên đăng nhập phải chứa cả chữ và số!');
       return;
     }

    // Kiểm tra email hợp lệ
    const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    if (!emailRegex.test(this.email)) {
      alert('Địa chỉ email không hợp lệ!');
      return;
    }

    // Kiểm tra số điện thoại (chỉ chứa số và ít nhất 10 ký tự)
    const phoneRegex = /^\d{10}$/;
    if (!phoneRegex.test(this.phone)) {
      alert('Số điện thoại phải chứa ít nhất 10 ký tự số!');
      return;
    }

    // Kiểm tra mật khẩu và xác nhận mật khẩu khớp nhau
    if (this.passWord !== this.confirmPassword) {
      alert('Mật khẩu không khớp!');
      return;
    }
    // Kiểm tra mật khẩu không trống
    if (!this.passWord || !this.confirmPassword) {
      alert('Mật khẩu và xác nhận mật khẩu không được bỏ trống!');
      return;
    }

    // Kiểm tra đồng ý với điều khoản bảo mật
    if (!this.isAgreed) {
      alert('Bạn cần đồng ý với điều khoản bảo mật để tiếp tục đăng ký.');
      return;
    }
  
    // Gán dữ liệu cho object dky
    this.dky.userName = this.userName;
    this.dky.email = this.email;
    this.dky.phone = this.phone;
    this.dky.address = this.address;
    this.dky.isAgreed = this.isAgreed;
    this.dky.passWord = this.passWord;
    this.dky.confirmPassword = this.confirmPassword;

    // Gửi dữ liệu qua service
    this.service.dangKy(this.dky).subscribe(
      (data: any) => {
        if (data.status === 'success') {
          alert("Đăng ký thành công!");
          window.location.href = "http://localhost:4300/login";
        } else {
          alert("Tên đăng nhập đã tồn tại hoặc có lỗi xảy ra. Vui lòng thử lại.");
        }
      },
      (error: any) => {
        console.error('Error during registration:', error);
        alert("Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại sau.");
      }
    );
  }
}

