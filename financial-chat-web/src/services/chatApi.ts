import { API_BASE_URL } from "../config/api";

export interface SendMessageRequest {
  roomId: string;
  userId: string;
  text: string;
}

export async function sendMessage(req: SendMessageRequest): Promise<void> {
  const res = await fetch(`${API_BASE_URL}/api/chat/send`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(req),
  });

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`Error al enviar mensaje: ${res.status} ${text}`);
  }
}
