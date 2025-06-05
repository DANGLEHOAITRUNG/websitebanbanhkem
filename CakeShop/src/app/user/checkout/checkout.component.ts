import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CakeshopService, Order } from '@app/cakeshop.service';

@Component({
  selector: 'app-checkout',
  standalone: false,
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
  cartItems: any[] = [];
  TotalAmount: number = 0;
  userId: number = 0;
  
  // Thêm biến mới
  checkoutForm: FormGroup;
  isSubmitting: boolean = false;
  orderSuccess: boolean = false;
  newOrderId: number | null = null;
  shippingFee: number = 0;


  selectedFile: File | null = null
  imagePreview: string | null = null
  uploadProgress = 0
  isUploading = false
  uploadedImageUrl: string | null = null
  
  constructor(
    private router: Router, 
    private service: CakeshopService,
    private fb: FormBuilder
  ) {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state as {
      cartItems: any[];
      totalAmount: number;
    };

    this.cartItems = state?.cartItems || [];
    this.TotalAmount = state?.totalAmount || 0;
    
    // Khởi tạo form
    this.checkoutForm = this.fb.group({
      FullName: [''],
      Phone: [''],
      Address: [''],
      PaymentMethod: [''],
    });
  }

  ngOnInit(): void {
    const storedId = localStorage.getItem("userId");
    this.userId = storedId ? parseInt(storedId, 10) : 0;
    
    if (this.userId && (!this.cartItems || this.cartItems.length === 0)) {
      this.loadCart();
    }
  
  }
  
  loadCart() {
    this.service.getCart(this.userId).subscribe(response => {
      if (response.status === 'success') {
        this.cartItems = response.data;
        this.calculateTotal();
        this.service.setCartCount(this.cartItems.length);
      }
    });
  }
  
  calculateTotal() {
    this.TotalAmount = this.cartItems.reduce((sum, item) => sum + item.TotalAmount, 0);
  }
  
  placeOrder() {
    if (this.checkoutForm.invalid) {
      // Nếu không hợp lệ, đánh dấu tất cả field là touched để hiển thị lỗi
      Object.keys(this.checkoutForm.controls).forEach(key => {
        const control = this.checkoutForm.get(key);
        control?.markAsTouched();
      });
      return;
    }

     // Kiểm tra nếu chọn chuyển khoản thì phải có ảnh chứng minh
    if (this.checkoutForm.value.PaymentMethod === "bank" && !this.uploadedImageUrl) {
      alert("Vui lòng tải lên ảnh chứng minh thanh toán cho phương thức chuyển khoản.")
      return
    }
  
    if (!this.cartItems || this.cartItems.length === 0) {
      alert('Giỏ hàng của bạn đang trống.');
      return;
    }
    
    this.isSubmitting = true;
    
    const formValues = this.checkoutForm.value;
    
    const orderData = {
      userId: this.userId,
      FullName: formValues.FullName,
      Address: formValues.Address,
      Phone: formValues.Phone,
      PaymentMethod: formValues.PaymentMethod,
      TotalAmount: this.TotalAmount + this.shippingFee,
      Status: "Pending",
      orderDate: new Date().toISOString(),
       PaymentProofImage: this.uploadedImageUrl,
      items: this.cartItems.map(item => ({
        productId: item.productId,
        quantity: item.quantity
      }))
    };
  
    this.service.createOrder(this.userId, orderData).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          this.service.updateCartCount(0);
          this.orderSuccess = true;
          this.newOrderId = response.data.orderId;
        } else {
          alert('Lỗi: ' + response.message);
        }
        this.isSubmitting = false;
      },
      error: (error) => {
        console.error('Lỗi tạo đơn hàng:', error);
        alert('Không thể đặt hàng. Vui lòng thử lại sau.');
        this.isSubmitting = false;
      }
    });
  }

 // Xử lý khi người dùng chọn file ảnh
  onFileSelected(event: any) {
    const file = event.target.files[0]
    if (file) {
      // Kiểm tra định dạng file
      const allowedTypes = ["image/jpeg", "image/jpg", "image/png", "image/gif"]
      if (!allowedTypes.includes(file.type)) {
        alert("Chỉ chấp nhận file ảnh (JPG, PNG, GIF)")
        return
      }

      // Kiểm tra kích thước file (tối đa 5MB)
      if (file.size > 5 * 1024 * 1024) {
        alert("File không được vượt quá 5MB")
        return
      }

      this.selectedFile = file

      // Tạo preview ảnh
      const reader = new FileReader()
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result
      }
      reader.readAsDataURL(file)

      // Reset trạng thái upload
      this.uploadedImageUrl = null
      this.uploadProgress = 0
    }
  }

  // Upload ảnh lên server
  uploadImage() {
    if (!this.selectedFile) return

    this.isUploading = true
    this.uploadProgress = 0

    // Tạo FormData để gửi file
    const formData = new FormData()
    formData.append("file", this.selectedFile)

    // Sử dụng XMLHttpRequest để theo dõi tiến trình upload
    const xhr = new XMLHttpRequest()

    // Lắng nghe sự kiện tiến trình upload
    xhr.upload.addEventListener("progress", (event) => {
      if (event.lengthComputable) {
        this.uploadProgress = Math.round((event.loaded / event.total) * 100)
      }
    })

    // Xử lý khi upload hoàn thành
    xhr.addEventListener("load", () => {
      this.isUploading = false
      if (xhr.status === 200) {
        const response = JSON.parse(xhr.responseText)
        if (response.status === "success") {
          this.uploadedImageUrl = response.fileUrl
          this.uploadProgress = 100
        } else {
          alert("Lỗi upload: " + response.message)
        }
      } else {
        alert("Lỗi upload file")
      }
    })

    // Xử lý lỗi upload
    xhr.addEventListener("error", () => {
      this.isUploading = false
      alert("Lỗi kết nối khi upload file")
    })

    // Gửi request upload
    xhr.open("POST", "http://localhost:5130/api/Orders/UploadPaymentProof")
    xhr.send(formData)
  }
  
  // Các phương thức mới
  get formControls() {
    return this.checkoutForm.controls;
  }
  
  viewOrderDetails() {
    this.router.navigate(['/checkout-detail']);
  }
  
  continueShopping() {
    this.router.navigate(['/']);
  }
  
  getPaymentMethodName(method: string): string {
    return method === 'cod' ? 'Thanh toán khi nhận hàng' : 'Chuyển khoản ngân hàng';
  }
  
  selectPaymentMethod(method: string) {
    this.checkoutForm.patchValue({ PaymentMethod: method });
  }
  
  backToCart() {
    this.router.navigate(['/cart']);
  }
}
