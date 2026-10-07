import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateTeachers } from './update-teachers';

describe('UpdateTeachers', () => {
  let component: UpdateTeachers;
  let fixture: ComponentFixture<UpdateTeachers>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateTeachers],
    }).compileComponents();

    fixture = TestBed.createComponent(UpdateTeachers);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
