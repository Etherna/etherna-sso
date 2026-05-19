import qrcode from "qrcode-generator"

function renderInto(element: HTMLElement, text: string): void {
    const qr = qrcode(0, "M")
    qr.addData(text)
    qr.make()
    element.innerHTML = qr.createSvgTag({ cellSize: 5, margin: 2, scalable: true })
}

function renderAll(): void {
    document.querySelectorAll<HTMLElement>("[data-qrcode]").forEach(el => {
        const text = el.getAttribute("data-text")
        if (text) renderInto(el, text)
    })
}

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", renderAll)
} else {
    renderAll()
}

export {}
