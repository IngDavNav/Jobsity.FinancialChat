import type { User } from "./User";

export interface Session {
  user: User;
  roomId: string;
  roomName: string;
}
