import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CakeshopService } from '../../cakeshop.service';

@Component({
  selector: 'app-customer-detail',
  standalone: false,
  templateUrl: './customer-detail.component.html',
  styleUrls: ['./customer-detail.component.css'],
})
export class CustomerDetailComponent implements OnInit {
  customer: any;

  constructor(
    private route: ActivatedRoute,
    private service: CakeshopService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    this.service.getCustomersById(id).subscribe(data => {
      this.customer = data[0];
    });
  }
}
