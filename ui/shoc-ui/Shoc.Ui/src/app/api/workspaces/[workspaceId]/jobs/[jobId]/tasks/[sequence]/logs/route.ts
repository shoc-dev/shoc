import { auth } from '@/addons/auth';
import { getJwt } from '@/addons/auth/actions';
import { shocClient } from '@/clients/shoc';
import WorkspaceJobTasksClient from '@/clients/shoc/job/workspace-job-tasks-client';
import { NextRequest } from 'next/server';

export async function GET(req: NextRequest, { params }: { params: Promise<{ workspaceId: string, jobId: string, sequence: string }> }) {
    await auth()
    const { workspaceId, jobId, sequence } = await params;
    const jwt = await getJwt();

    const { url } = shocClient(WorkspaceJobTasksClient).getLogsBySequenceUrl(workspaceId, jobId, sequence)

    const externalResponse = await fetch(url, {
        method: 'GET',
        headers: {
            ...req.headers,
            'Authorization': `Bearer ${jwt?.actualAccessToken}`,
            'Cookie': ''
        }
    });

    if (!externalResponse.body) {
        return new Response(externalResponse.body, { status: externalResponse.status });
    }

    return new Response(externalResponse.body, {
        status: externalResponse.status,
        headers: {
            ...externalResponse.headers
        },
    });
}