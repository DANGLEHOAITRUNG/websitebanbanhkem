import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { CakeshopService } from '../../cakeshop.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-menu',
  standalone: false,
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit {

  products: any[] = []

 
  filteredProducts: any[] = []

 
  categories: any[] = []

  
  searchTerm = ""
  selectedCategoryId = ""
  sortOption = "default"

  currentPage = 1
  itemsPerPage = 8
  totalPages = 1

  isLoading = false


  userId = 0

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: CakeshopService,
  ) {}

  ngOnInit(): void {
    // Get user ID from localStorage
    const storedId = localStorage.getItem("userId")
    this.userId = storedId ? Number.parseInt(storedId, 10) : 0


    this.loadCategories()

    this.route.queryParams.subscribe((params) => {
      this.searchTerm = params["search"] || ""
      this.selectedCategoryId = params["category"] || ""
      this.currentPage = Number.parseInt(params["page"] || "1", 10)


      this.loadProducts()
    })
  }

  loadCategories(): void {
    this.service.getAllCategories().subscribe({
      next: (res) => {
        this.categories = res
      },
      error: (err) => {
        console.error("Lỗi khi tải danh mục:", err)
        this.categories = []
      },
    })
  }

  loadProducts(): void {
    this.isLoading = true
    if (this.selectedCategoryId) {
      this.service.getProductsByCategory(this.selectedCategoryId).subscribe({
        next: (res) => {
          this.handleProductsResponse(res)
        },
        error: (err) => {
          console.error("Lỗi khi tải sản phẩm theo danh mục:", err)
          this.products = []
          this.filteredProducts = []
          this.isLoading = false
        },
      })
    }
    
    else if (this.searchTerm) {
      this.service.searchProducts(this.searchTerm).subscribe({
        next: (res) => {
          this.handleProductsResponse(res)
        },
        error: (err) => {
          console.error("Lỗi khi tìm kiếm sản phẩm:", err)
          this.products = []
          this.filteredProducts = []
          this.isLoading = false
        },
      })
    }
    
    else {
      this.service.getAllProducts().subscribe({
        next: (res) => {
          this.handleProductsResponse(res)
        },
        error: (err) => {
          console.error("Lỗi khi tải sản phẩm:", err)
          this.products = []
          this.filteredProducts = []
          this.isLoading = false
        },
      })
    }
  }

  handleProductsResponse(res: any[]): void {
    this.products = res.map((product) => ({
      ...product,
      IsNew: Math.random() > 0.7, // Randomly mark some products as new (for demo)
    }))

    this.applySorting()

    this.applyPagination()

    this.isLoading = false
  }

  applySorting(): void {
    switch (this.sortOption) {
      case "price-asc":
        this.products.sort((a, b) => a.Price - b.Price)
        break
      case "price-desc":
        this.products.sort((a, b) => b.Price - a.Price)
        break
      case "name-asc":
        this.products.sort((a, b) => a.ProductName.localeCompare(b.ProductName))
        break
      case "name-desc":
        this.products.sort((a, b) => b.ProductName.localeCompare(a.ProductName))
        break
      default:
        
        this.products.sort((a, b) => a.ProductID - b.ProductID)
        break
    }
  }

  applyPagination(): void {
    
    this.totalPages = Math.ceil(this.products.length / this.itemsPerPage)

   
    if (this.currentPage < 1) this.currentPage = 1
    if (this.currentPage > this.totalPages) this.currentPage = this.totalPages

   
    const startIndex = (this.currentPage - 1) * this.itemsPerPage
    this.filteredProducts = this.products.slice(startIndex, startIndex + this.itemsPerPage)
  }

  sortProducts(event: any): void {
    this.sortOption = event.target.value
    this.applySorting()
    this.applyPagination()

    this.updateUrlParams()
  }

  filterByCategory(event: any): void {
    this.selectedCategoryId = event.target.value
    this.currentPage = 1 
    this.updateUrlParams()
    this.loadProducts()
  }

  searchProducts(): void {
    this.currentPage = 1 
    this.updateUrlParams()
    this.loadProducts()
  }

  resetFilters(): void {
    this.searchTerm = ""
    this.selectedCategoryId = ""
    this.sortOption = "default"
    this.currentPage = 1

    this.updateUrlParams()
    this.loadProducts()
  }

  goToPage(page: number): void {
    this.currentPage = page
    this.updateUrlParams()
    this.applyPagination()

    document.getElementById("products")?.scrollIntoView({ behavior: "smooth" })
  }

  getPageNumbers(): number[] {
    const pages: number[] = []
    const maxPagesToShow = 5

    if (this.totalPages <= maxPagesToShow) {
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i)
      }
    } else {
      if (this.currentPage <= 3) {
        for (let i = 1; i <= 4; i++) {
          pages.push(i)
        }
        pages.push(this.totalPages)
      } else if (this.currentPage >= this.totalPages - 2) {
        pages.push(1)
        for (let i = this.totalPages - 3; i <= this.totalPages; i++) {
          pages.push(i)
        }
      } else {
        pages.push(1)
        for (let i = this.currentPage - 1; i <= this.currentPage + 1; i++) {
          pages.push(i)
        }
        pages.push(this.totalPages)
      }
    }

    return pages
  }

  updateUrlParams(): void {
    const queryParams: any = {}

    if (this.searchTerm) queryParams.search = this.searchTerm
    if (this.selectedCategoryId) queryParams.category = this.selectedCategoryId
    if (this.currentPage > 1) queryParams.page = this.currentPage
    if (this.sortOption !== "default") queryParams.sort = this.sortOption

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: "merge",
    })
  }

  addToCart(product: any): void {
    if (this.userId === 0) {
      alert("Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!")
      return
    }

    this.service.addToCart(this.userId, product.ProductID, 1).subscribe((response) => {
      if (response.status === "success") {
        alert("Sản phẩm đã thêm vào giỏ hàng!")

        this.service.refreshCartCount(this.userId)
      } else {
        alert("Lỗi: " + response.message)
      }
    })
  }
}
