import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateSubject } from './create-subject';

describe('CreateSubject', () => {
  let component: CreateSubject;
  let fixture: ComponentFixture<CreateSubject>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateSubject],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateSubject);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
