export function selectedFileName(input) {
    return input.files?.[0]?.name ?? "";
}

export function preview(input) {
    const file = input.files?.[0];

    return file
        ? URL.createObjectURL(file)
        : "";
}

export async function uploadInput(input, kind) {
    const file = input.files?.[0];

    if (!file) {
        throw new Error("Selecione um arquivo primeiro.");
    }

    const data = new FormData();
    data.append("file", file);

    const response = await fetch(
        `/admin/upload/${encodeURIComponent(kind)}`,
        {
            method: "POST",
            body: data
        }
    );

    const text = await response.text();

    let payload;

    try {
        payload = JSON.parse(text);
    } catch {
        payload = text;
    }

    if (!response.ok) {
        throw new Error(
            payload?.detail ??
            (typeof payload === "string"
                ? payload
                : "Falha ao enviar arquivo.")
        );
    }

    return payload.url;
}