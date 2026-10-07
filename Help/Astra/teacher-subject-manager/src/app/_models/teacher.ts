import { Subject } from './subject';

export class Teacher {
  // id, name, neptun, birthy, image, creatorname, teachedSubjects
  id: string = '';
  name: string = '';
  neptun: string = '';
  birthYear: number = 0;
  image: string = '';
  creatorName: string = '';
  teachedSubjects: Array<Subject> = [];

  public createSubjects(subjectList: Array<any>) {
    subjectList.map((x: any) => {
      const s = new Subject();

      s.id = x.id;
      s.name = x.name;
      s.neptun = x.neptun;
      s.credit = x.credit;
      s.image = x.image;
      s.creatorName = x.creatorName;
      s.exam = x.exam;
      s.registeredStudents = x.registeredStudents;

      this.teachedSubjects.push(s);
    });
  }
}
