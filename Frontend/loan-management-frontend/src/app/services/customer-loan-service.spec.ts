import { TestBed } from '@angular/core/testing';

import { CustomerLoanService } from './customer-loan-service';

describe('CustomerLoanService', () => {
  let service: CustomerLoanService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CustomerLoanService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
