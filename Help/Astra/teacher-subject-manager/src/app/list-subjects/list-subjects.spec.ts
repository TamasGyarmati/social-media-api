import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListSubjects } from './list-subjects';

describe('ListSubjects', () => {
  let component: ListSubjects;
  let fixture: ComponentFixture<ListSubjects>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListSubjects],
    }).compileComponents();

    fixture = TestBed.createComponent(ListSubjects);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
