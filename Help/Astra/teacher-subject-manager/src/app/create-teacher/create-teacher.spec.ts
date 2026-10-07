import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateTeacher } from './create-teacher';

describe('CreateTeacher', () => {
  let component: CreateTeacher;
  let fixture: ComponentFixture<CreateTeacher>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateTeacher],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateTeacher);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
