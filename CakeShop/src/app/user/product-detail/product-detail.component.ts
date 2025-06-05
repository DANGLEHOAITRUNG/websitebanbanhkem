import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CakeshopService } from '../../cakeshop.service';

@Component({
  selector: 'app-product-detail',
  standalone: false,
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css'],
})
export class ProductDetailComponent  implements OnInit {
  product: any // Dữ liệu chi tiết sản phẩm
  quantity = 1 // Số lượng mặc định là 1
  userId = 0
  activeTab = "description"

  constructor(
    private service: CakeshopService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const storedId = localStorage.getItem("userId")
    this.userId = storedId ? Number.parseInt(storedId, 10) : 0
    const productId = this.route.snapshot.paramMap.get("id")
    if (productId) {
      this.loadProductDetail(Number(productId))
    }

    // Set up tab functionality
    setTimeout(() => {
      this.setupTabs()
    }, 500)
  }

  loadProductDetail(id: number) {
    this.service.getProductById(id).subscribe(
      (data) => {
        this.product = data
      },
      (error) => {
        console.error("Lỗi khi tải chi tiết sản phẩm:", error)
      },
    )
  }

  addToCart(product: any): void {
    if (product.Quantity <= 0) {
      alert("Sản phẩm đã hết hàng!")
      return
    }

    this.service.addToCart(this.userId, product.ProductID, this.quantity).subscribe((response) => {
      if (response.status === "success") {
        alert("Sản phẩm đã thêm vào giỏ hàng!")

        // Cập nhật số lượng giỏ hàng trên header
        this.service.refreshCartCount(this.userId)
      } else {
        alert("Lỗi: " + response.message)
      }
    })
  }

  // Phương thức tăng số lượng
  increaseQuantity(): void {
    if (this.quantity < this.product?.Quantity) {
      this.quantity++
    }
  }

  // Phương thức giảm số lượng
  decreaseQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--
    }
  }

  goBack() {
    this.router.navigate(["/user/home/menu"])
  }

  // Thiết lập tabs
  setupTabs() {
    const tabButtons = document.querySelectorAll(".tab-btn")
    const tabContents = document.querySelectorAll(".tab-content")

    tabButtons.forEach((button) => {
      button.addEventListener("click", () => {
        // Remove active class from all buttons and contents
        tabButtons.forEach((btn) => btn.classList.remove("active"))
        tabContents.forEach((content) => content.classList.remove("active"))

        // Add active class to clicked button
        button.classList.add("active")

        // Get the tab to show
        const tabToShow = button.getAttribute("data-tab")

        // Show the corresponding tab content
        if (tabToShow) {
          const content = document.getElementById(tabToShow)
          if (content) {
            content.classList.add("active")
          }
        }
      })
    })
  }
}
