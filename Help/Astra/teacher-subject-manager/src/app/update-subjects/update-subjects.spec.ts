import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateSubjects } from './update-subjects';

describe('UpdateSubjects', () => {
  let component: UpdateSubjects;
  let fixture: ComponentFixture<UpdateSubjects>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateSubjects],
    }).compileComponents();

    fixture = TestBed.createComponent(UpdateSubjects);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
