import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CommentItem } from './comment-item';

describe('CommentItem', () => {
  let component: CommentItem;
  let fixture: ComponentFixture<CommentItem>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommentItem],
    }).compileComponents();

    fixture = TestBed.createComponent(CommentItem);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
