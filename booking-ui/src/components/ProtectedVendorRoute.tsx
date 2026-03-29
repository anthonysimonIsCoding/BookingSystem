import { Navigate } from "react-router-dom";
import { useAuth } from "../hook/useAuth";

function ProtectedVendorRoute({ children }: { children: React.ReactNode }) {
    const { isVendorLoggedIn } = useAuth();

    if (!isVendorLoggedIn) {
        return <Navigate to="/vendor/login" replace />;
    }

    return <>{children}</>;
}

export default ProtectedVendorRoute;