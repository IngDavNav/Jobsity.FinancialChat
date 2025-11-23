import React, { useState } from "react";
import type { Session } from "./types";
import { AuthScreen, ChatScreen } from "./screens";

export const App: React.FC = () => {
  const [session, setSession] = useState<Session | null>(null);

  if (!session) {
    return <AuthScreen onSessionReady={setSession} />;
  }

  return (
    <ChatScreen
      user={session.user}
      roomId={session.roomId}
      roomName={session.roomName}
    />
  );
};
