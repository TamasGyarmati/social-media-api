import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConnectTeacherToSubject } from './connect-teacher-to-subject';

describe('ConnectTeacherToSubject', () => {
  let component: ConnectTeacherToSubject;
  let fixture: ComponentFixture<ConnectTeacherToSubject>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConnectTeacherToSubject],
    }).compileComponents();

    fixture = TestBed.createComponent(ConnectTeacherToSubject);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
