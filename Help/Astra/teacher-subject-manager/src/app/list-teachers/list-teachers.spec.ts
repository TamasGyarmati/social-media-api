import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListTeachers } from './list-teachers';

describe('ListTeachers', () => {
  let component: ListTeachers;
  let fixture: ComponentFixture<ListTeachers>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListTeachers],
    }).compileComponents();

    fixture = TestBed.createComponent(ListTeachers);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
