import { NgModule } from '@angular/core';
import { BrowserModule, provideClientHydration, withEventReplay } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './LoginRegister/login/login.component';
import { RegisterComponent } from './LoginRegister/register/register.component';
import { HomeComponent } from './user/home/home.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { provideHttpClient, withFetch, withInterceptorsFromDi } from '@angular/common/http';
import { CustomerManagementComponent } from './admin/customer-management/customer-management.component';
import { CustomerDetailComponent } from './admin/customer-detail/customer-detail.component';
import { CustomerAddComponent } from './admin/customer-add/customer-add.component';
import { CustomerEditComponent } from './admin/customer-edit/customer-edit.component';
import { AdminHomeComponent } from './admin/admin-home/admin-home.component';

import { UserFooterComponent } from './user/user-footer/user-footer.component';
import { ProductManagementComponent } from './admin/product-management/product-management.component';
import { ProductAddComponent } from './admin/product-add/product-add.component';
import { ProductEditComponent } from './admin/product-edit/product-edit.component';
import { CategoryManagementComponent } from './admin/category-management/category-management.component';
import { OrderManagementComponent } from './admin/order-management/order-management.component';
import { OrderEditComponent } from './admin/order-edit/order-edit.component';
import { UserProfileComponent } from './user/user-profile/user-profile.component';
import { UserHeaderComponent } from './user/user-header/user-header.component';
import { MenuComponent } from './user/menu/menu.component';
import { ProductDetailComponent } from './user/product-detail/product-detail.component';
import { AboutComponent } from './about/about.component';
import { ContactComponent } from './contact/contact.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { UserCartComponent } from './user/user-cart/user-cart.component';
import { CheckoutComponent } from './user/checkout/checkout.component';
import { CheckoutDetailComponent } from './user/checkout-detail/checkout-detail.component';
import { DatePipe } from '@angular/common';


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    HomeComponent,
    CustomerManagementComponent,
    CustomerDetailComponent,
    CustomerAddComponent,
    CustomerEditComponent,
    AdminHomeComponent,
    UserFooterComponent,
    ProductManagementComponent,
    ProductAddComponent,
    ProductEditComponent,
    CategoryManagementComponent,
    OrderManagementComponent,
    OrderEditComponent,
    UserHeaderComponent,
    MenuComponent,
    ProductDetailComponent,
    AboutComponent,
    ContactComponent,
    UserProfileComponent,
    UserCartComponent,
    CheckoutComponent,
    CheckoutDetailComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    NgxPaginationModule,
    ReactiveFormsModule
  ],
  providers: [DatePipe,
    provideHttpClient(withFetch(), withInterceptorsFromDi())
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
