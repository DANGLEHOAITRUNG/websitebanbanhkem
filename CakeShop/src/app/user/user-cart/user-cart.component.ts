import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CakeshopService } from '@app/cakeshop.service';

@Component({
  selector: 'app-user-cart',
  standalone: false,
  templateUrl: './user-cart.component.html',
  styleUrls: ['./user-cart.component.css']
})
export class UserCartComponent implements OnInit {
  cartItems: any[] = []
  totalAmount = 0
  userId = 0


  isLoading = false

  constructor(
    private service: CakeshopService,
    private router: Router,
  ) {}

  ngOnInit() {
    const storedId = localStorage.getItem("userId")
    this.userId = storedId ? Number.parseInt(storedId, 10) : 0
    this.loadCart()
  }

  loadCart() {
    this.isLoading = true
    this.service.getCart(this.userId).subscribe((response) => {
      if (response.status === "success") {
        this.cartItems = response.data

        // Thêm thuộc tính MaxQuantity cho mỗi sản phẩm (giả định là số lượng tối đa có thể mua)
        this.cartItems.forEach((item) => {
          item.MaxQuantity = item.AvailableQuantity || 99 // Sử dụng số lượng có sẵn nếu có, nếu không thì mặc định là 99
        })

        this.calculateTotal()
        this.service.setCartCount(this.cartItems.length)
      }
      this.isLoading = false
    })
  }

  updateQuantity(productId: number, newQuantity: number) {
    if (newQuantity < 1) return // Không cho phép số lượng nhỏ hơn 1

    // Tìm sản phẩm trong giỏ hàng
    const item = this.cartItems.find((item) => item.ProductID === productId)
    if (!item) return

    // Kiểm tra số lượng tối đa
    if (item.MaxQuantity && newQuantity > item.MaxQuantity) {
      alert(`Chỉ còn ${item.MaxQuantity} sản phẩm trong kho!`)
      newQuantity = item.MaxQuantity
    }

    this.isLoading = true
    this.service.updateCart(this.userId, productId, newQuantity).subscribe((response) => {
      if (response.status === "success") {
        // Cập nhật số lượng trực tiếp trên UI
        item.Quantity = newQuantity
        item.TotalAmount = item.Price * newQuantity

        this.calculateTotal()
      } else {
        alert(response.message)
      }
      this.isLoading = false
    })
  }

  // Tính tổng số tiền giỏ hàng
  calculateTotal() {
    this.totalAmount = this.cartItems.reduce((sum, item) => sum + item.Price * item.Quantity, 0)
  }



 
  // Hàm xóa sản phẩm khỏi giỏ hàng
  removeFromCart(productId: number) {
    if (confirm("Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?")) {
      this.isLoading = true
      this.service.DeleteFromCart(this.userId, productId).subscribe((response) => {
        if (response.status === "success") {
          this.cartItems = this.cartItems.filter((item) => item.ProductID !== productId)
          this.calculateTotal()
          this.service.setCartCount(this.cartItems.length)
        }
        this.isLoading = false
      })
    }
  }

  goBack() {
    this.router.navigate(["/user/home/menu"])
  }

  goToCheckout() {
    if (this.cartItems.length === 0) {
      alert("Giỏ hàng của bạn đang trống!")
      return
    }

    this.router.navigate(["/checkout"], {
      state: {
        cartItems: this.cartItems,
        totalAmount: this.totalAmount,
      },
    })
  }
}
