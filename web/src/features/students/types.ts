export type StudentStatus = "active" | "archived";

export interface StudentListItem {
  id: string;
  code: string;
  fullName: string;
  phone: string;
  gradeLevel: string | null;
  status: StudentStatus;
  activeClassCount: number;
  updatedAt: string;
}

export interface StudentEnrollment {
  id: string;
  classId: string;
  className: string;
  subject: string;
  scheduleText: string | null;
  enrolledOn: string;
  status: "active" | "withdrawn";
  withdrawnOn: string | null;
}

export interface StudentDetail {
  id: string;
  code: string;
  fullName: string;
  phone: string;
  email: string | null;
  guardianName: string | null;
  guardianPhone: string | null;
  dateOfBirth: string | null;
  school: string | null;
  gradeLevel: string | null;
  joinedOn: string;
  notes: string | null;
  status: StudentStatus;
  createdAt: string;
  updatedAt: string;
  enrollments: StudentEnrollment[];
}

export interface StudentListParams {
  search?: string;
  status?: "active" | "archived" | "all";
  sort?: "name" | "code" | "recent";
  page?: number;
  pageSize?: number;
  excludeClassId?: string;
}

export interface SaveStudentPayload {
  fullName: string;
  phone: string;
  email: string | null;
  guardianName: string | null;
  guardianPhone: string | null;
  dateOfBirth: string | null;
  school: string | null;
  gradeLevel: string | null;
  joinedOn: string;
  notes: string | null;
  acknowledgeDuplicate?: boolean;
}
