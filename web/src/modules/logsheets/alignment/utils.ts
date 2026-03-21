// No good js library for this
// Source: Gemini

export const getHomographyMatrix = (
    src: { x: number; y: number }[],
    dst: { x: number; y: number }[]
) => {
    const t = solveHomography(src, dst);

    return `matrix3d(
    ${t[0]}, ${t[3]}, 0, ${t[6]},
    ${t[1]}, ${t[4]}, 0, ${t[7]},
    0, 0, 1, 0,
    ${t[2]}, ${t[5]}, 0, 1
  )`;
};

const solveHomography = (
    src: { x: number; y: number }[],
    dst: { x: number; y: number }[]
) => {
    const A: number[][] = [];
    const b: number[] = [];

    for (let i = 0; i < 4; i++) {
        const s = src[i];
        const d = dst[i];

        A.push([s.x, s.y, 1, 0, 0, 0, -d.x * s.x, -d.x * s.y]);
        b.push(d.x);

        A.push([0, 0, 0, s.x, s.y, 1, -d.y * s.x, -d.y * s.y]);
        b.push(d.y);
    }

    const h = gaussianElimination(A, b);

    return h;
};

const gaussianElimination = (A: number[][], b: number[]) => {
    const n = A.length;

    for (let i = 0; i < n; i++) {
        A[i].push(b[i]);
    }

    for (let i = 0; i < n; i++) {
        let maxRow = i;
        for (let k = i + 1; k < n; k++) {
            if (Math.abs(A[k][i]) > Math.abs(A[maxRow][i])) {
                maxRow = k;
            }
        }

        [A[i], A[maxRow]] = [A[maxRow], A[i]];

        for (let k = i + 1; k < n; k++) {
            const factor = A[k][i] / A[i][i];
            for (let j = i; j <= n; j++) {
                A[k][j] -= factor * A[i][j];
            }
        }
    }

    const x = new Array(n).fill(0);
    for (let i = n - 1; i >= 0; i--) {
        let sum = 0;
        for (let j = i + 1; j < n; j++) {
            sum += A[i][j] * x[j];
        }
        x[i] = (A[i][n] - sum) / A[i][i];
    }

    return x;
};
