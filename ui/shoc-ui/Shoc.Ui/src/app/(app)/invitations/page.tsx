import type { Metadata } from 'next'
import getIntl from "@/i18n/get-intl";
import InvitationsClientPage from './_components/invitations-client-page';

export const dynamic = 'force-dynamic';

export async function generateMetadata(): Promise<Metadata> {
   
    const intl = await getIntl();

    return {
      title: intl.formatMessage({id: 'workspaces.invitations'})
    }
  }

export default async function InvitationsPage() {
    return <InvitationsClientPage />
}
