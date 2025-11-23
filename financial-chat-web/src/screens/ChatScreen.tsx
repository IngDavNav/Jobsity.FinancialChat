import React, { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";

import type { User } from "../types/User";
import type { ChatMessage } from "../types/ChatMessage";
import { API_BASE_URL } from "../config/api";

interface ChatScreenProps {
  user: User;
  roomId: string;
  roomName: string;
}

export const ChatScreen: React.FC<ChatScreenProps> = ({
  user,
  roomId,
  roomName,
}) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [text, setText] = useState("");
  const [connecting, setConnecting] = useState(true);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const bottomRef = useRef<HTMLDivElement | null>(null);

    useEffect(() => {
    if (bottomRef.current) {
      bottomRef.current.scrollIntoView({ behavior: "smooth" });
    }
  }, [messages]);

    useEffect(() => {
    let isCancelled = false;

    const connect = async () => {
      try {
        setConnecting(true);
        setError(null);

        const historyRes = await fetch(
          `${API_BASE_URL}/api/chat/history/${roomId}`
        );

        if (historyRes.ok) {
          const history: ChatMessage[] = await historyRes.json();
          if (!isCancelled) {
            setMessages(history);
          }
        } else {
          console.warn("No se pudo cargar historial de mensajes");
        }

        const connection = new signalR.HubConnectionBuilder()
          .withUrl(`${API_BASE_URL}/chatHub`, {
            accessTokenFactory: () => user.token,
          })
          .withAutomaticReconnect()
          .build();

        connection.on("ReceiveMessage", (msg: ChatMessage) => {
          setMessages((prev) => {
            if (prev.some((m) => m.id === msg.id)) return prev;
            return [...prev, msg];
          });
        });

        await connection.start();
        await connection.invoke("JoinRoom", roomId);

        if (!isCancelled) {
          connectionRef.current = connection;
          setConnecting(false);
        }
      } catch (err: any) {
        console.error("Error conectando SignalR:", err);
        if (!isCancelled) {
          setError(err.message ?? "Error connecting to chat");
          setConnecting(false);
        }
      }
    };

    connect();

    return () => {
      isCancelled = true;
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, [user.id]);

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!text.trim()) return;

    try {
      setSending(true);
      setError(null);

      const res = await fetch(`${API_BASE_URL}/api/chat/send`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${user.token}`,
        },
        body: JSON.stringify({
          roomId: roomId,
          userId: user.id,
          text: text.trim(),
        }),
      });

      if (!res.ok) {
        const txt = await res.text();
        throw new Error(txt || "Error sending message");
      }

      setText("");
    } catch (err: any) {
      console.error(err);
      setError(err.message ?? "Unexpected error sending message");
    } finally {
      setSending(false);
    }
  };

  return (
    <div className="min-h-screen flex flex-col bg-emerald-50">
      <header className="px-4 py-3 bg-emerald-700 text-white flex items-center justify-between shadow-md">
        <h1 className="font-semibold text-sm sm:text-base">
          {roomName} - Jobsity Financial Chat
        </h1>
        <div className="text-xs sm:text-sm opacity-90">
          Logged in as <span className="font-medium">{user.userName}</span>
        </div>
      </header>

      <main className="flex-1 flex flex-col px-2 sm:px-4 py-3">
        {connecting && (
          <p className="text-xs text-emerald-700 mb-2">Connecting to chat…</p>
        )}
        {error && <p className="text-xs text-red-600 mb-2">{error}</p>}

        <div className="flex-1 overflow-y-auto rounded-2xl bg-emerald-100/60 px-2 sm:px-3 py-3 space-y-1">
          {messages.length === 0 && (
            <p className="text-xs text-emerald-700/70 text-center mt-4">
              No messages yet. Say hi 👋
            </p>
          )}

          {messages.map((m) => {
            const isMine = m.userId === user.id;
            const timeLabel = new Date(m.timeStamp).toLocaleTimeString([], {
              hour: "2-digit",
              minute: "2-digit",
            });

            return (
              <div
                key={m.id}
                className={`flex w-full ${
                  isMine ? "justify-end" : "justify-start"
                }`}
              >
                <div
                  className={[
                    "max-w-[75%] rounded-2xl px-3 py-2 text-sm leading-snug shadow-sm",
                    isMine
                      ? "bg-emerald-500 text-white"
                      : "bg-white text-slate-900",
                  ].join(" ")}
                >
                  <div className="flex items-baseline gap-2">
                    <span className="font-semibold text-xs">{m.userName}</span>
                    <span className="text-[10px] opacity-80">{timeLabel}</span>
                  </div>
                  <p className="mt-0.5 wrap-break-words">{m.text}</p>
                </div>
              </div>
            );
          })}

          <div ref={bottomRef} />
        </div>

        <form
          onSubmit={handleSend}
          className="mt-3 flex items-center gap-2 px-1"
        >
          <input
            type="text"
            value={text}
            onChange={(e) => setText(e.target.value)}
            className="flex-1 rounded-full border border-emerald-300 bg-white/90 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
            placeholder="Type a message… (use /stock=AAPL.US)"
          />
          <button
            type="submit"
            disabled={sending || connecting}
            className="rounded-full px-5 py-3 text-sm font-medium bg-emerald-600 text-white shadow-sm disabled:opacity-60 disabled:cursor-not-allowed hover:bg-emerald-700 transition-colors"
          >
            {sending ? "Sending..." : "Send"}
          </button>
        </form>
      </main>
    </div>
  );
};
