import React, { useState, useEffect } from "react";
import type { Session, User } from "../types";
import { API_BASE_URL } from "../config/api";

type AuthMode = "login" | "register";

interface AuthScreenProps {
  onSessionReady: (session: Session) => void;
}

interface Room {
  id: string;
  name: string;
}

export const AuthScreen: React.FC<AuthScreenProps> = ({ onSessionReady }) => {
  const [mode, setMode] = useState<AuthMode>("login");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [rooms, setRooms] = useState<Room[]>([]);
  const [selectedRoomId, setSelectedRoomId] = useState("");
  const [newRoomName, setNewRoomName] = useState("");
  const [loading, setLoading] = useState(false);
  const [roomsLoading, setRoomsLoading] = useState(false);
  const [newRoomLoading, setNewRoomLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

    useEffect(() => {
    const loadRooms = async () => {
      try {
        setRoomsLoading(true);
        setError(null);

        const res = await fetch(`${API_BASE_URL}/api/rooms`);
        if (!res.ok) {
          throw new Error("Failed to load rooms");
        }

        const data: Room[] = await res.json();
        setRooms(data);

        if (data.length > 0) {
          setSelectedRoomId(data[0].id);
        }
      } catch (err: any) {
        console.error(err);
        setError(err.message ?? "Error loading rooms");
      } finally {
        setRoomsLoading(false);
      }
    };

    loadRooms();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedUser = userName.trim();
    const trimmedPass = password.trim();
    const trimmedConfirm = confirmPassword.trim();

    if (!trimmedUser) {
      setError("User name is required.");
      return;
    }

    if (!trimmedPass) {
      setError("Password is required.");
      return;
    }

    if (mode === "register" && trimmedPass !== trimmedConfirm) {
      setError("Passwords do not match.");
      return;
    }

    if (!selectedRoomId) {
      setError("Please select a room.");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const endpoint = mode === "login" ? "login" : "register";
      const url = `${API_BASE_URL}/api/auth/${endpoint}`;

      const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userName: trimmedUser,
          password: trimmedPass,
        }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Authentication failed");
      }

      const data: { id: string; userName: string; token: string } =
        await res.json();

      const user: User = {
        id: data.id,
        userName: data.userName,
        token: data.token,
      };

      const room = rooms.find((r) => r.id === selectedRoomId);

      const session: Session = {
        user,
        roomId: selectedRoomId,
        roomName: room?.name ?? "Unknown room",
      };

      onSessionReady(session);
    } catch (err: any) {
      console.error(err);
      setError(err.message ?? "Unexpected error");
    } finally {
      setLoading(false);
    }
  };

  const toggleMode = () => {
    setMode((prev) => (prev === "login" ? "register" : "login"));
    setError(null);
  };

  const handleAddRoom = async () => {
    const trimmed = newRoomName.trim();
    if (!trimmed) {
      setError("Room name is required.");
      return;
    }

    setNewRoomLoading(true);
    setError(null);

    try {
      const res = await fetch(`${API_BASE_URL}/api/rooms`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name: trimmed }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Failed to create room");
      }

      const created: Room = await res.json();

      setRooms((prev) => [...prev, created]);
      setSelectedRoomId(created.id);
      setNewRoomName("");
    } catch (err: any) {
      console.error(err);
      setError(err.message ?? "Error creating room");
    } finally {
      setNewRoomLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-100">
      <div className="bg-white rounded-xl shadow-md p-6 w-full max-w-lg">
        <h1 className="text-xl font-semibold mb-4 text-center">
          Jobsity Financial Chat
        </h1>

        <div className="flex items-center justify-center gap-4 mb-4">
          <span
            className={`text-sm ${
              mode === "login"
                ? "text-slate-900 font-semibold"
                : "text-slate-400"
            }`}
          >
            Login
          </span>

          <button
            type="button"
            onClick={toggleMode}
            className={`relative w-14 h-7 rounded-full transition-colors ${
              mode === "register" ? "bg-slate-900" : "bg-slate-300"
            }`}
          >
            <span
              className={`absolute top-1 w-5 h-5 bg-white rounded-full shadow transition-transform ${
                mode === "register" ? "translate-x-1" : "-translate-x-7"
              }`}
            />
          </button>

          <span
            className={`text-sm ${
              mode === "register"
                ? "text-slate-900 font-semibold"
                : "text-slate-400"
            }`}
          >
            Register
          </span>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm mb-1">User name</label>
            <input
              type="text"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-slate-300"
              placeholder="Enter your user name"
              autoComplete="username"
            />
          </div>

          <div>
            <label className="block text-sm mb-1">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-slate-300"
              placeholder="Enter your password"
              autoComplete={
                mode === "login" ? "current-password" : "new-password"
              }
            />
          </div>

          {mode === "register" && (
            <div>
              <label className="block text-sm mb-1">Confirm password</label>
              <input
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-slate-300"
                placeholder="Repeat your password"
                autoComplete="new-password"
              />
            </div>
          )}

          <div>
            <label className="block text-sm mb-1">Room</label>
            {roomsLoading ? (
              <p className="text-xs text-slate-500">Loading rooms...</p>
            ) : rooms.length === 0 ? (
              <p className="text-xs text-red-600">
                No rooms available. Please create one below.
              </p>
            ) : (
              <select
                value={selectedRoomId}
                onChange={(e) => setSelectedRoomId(e.target.value)}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-slate-300"
              >
                {rooms.map((room) => (
                  <option key={room.id} value={room.id}>
                    {room.name}
                  </option>
                ))}
              </select>
            )}

            <div className="mt-2 flex gap-2">
              <input
                type="text"
                value={newRoomName}
                onChange={(e) => setNewRoomName(e.target.value)}
                className="flex-1 border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-slate-300"
                placeholder="New room name"
              />
              <button
                type="button"
                onClick={handleAddRoom}
                disabled={newRoomLoading}
                className="px-3 py-2 text-sm bg-slate-900 text-white rounded-lg disabled:opacity-60"
              >
                {newRoomLoading ? "Adding..." : "Add"}
              </button>
            </div>
          </div>

          {error && <p className="text-xs text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={loading || roomsLoading}
            className="w-full bg-slate-900 text-white rounded-lg py-2 text-sm disabled:opacity-60"
          >
            {loading
              ? "Processing..."
              : mode === "login"
              ? "Login"
              : "Create account"}
          </button>
        </form>
      </div>
    </div>
  );
};
