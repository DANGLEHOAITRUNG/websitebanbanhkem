import { TestBed } from '@angular/core/testing';

import { CakeshopService } from './cakeshop.service';

describe('CakeshopService', () => {
  let service: CakeshopService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CakeshopService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
