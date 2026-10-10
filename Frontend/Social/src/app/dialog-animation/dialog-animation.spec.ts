import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogAnimation } from './dialog-animation';

describe('DialogAnimation', () => {
  let component: DialogAnimation;
  let fixture: ComponentFixture<DialogAnimation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogAnimation],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogAnimation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
